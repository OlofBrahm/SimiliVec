import '../css/HomeScreen.css';
import similivecLogo from '../assets/similivec.svg';
import githubLogo from '../assets/github.svg';
import linkedinLogo from '../assets/linkedin.svg';

export function HomeScreen({ onContinue, githubUrl = '', linkedinUrl = '' }) {
  return (
    <div style={homeScreenStyle}>
      <div style={contentStyle}>
        <img src={similivecLogo} alt="SimiliVec" style={logoStyle} />
        <h1 style={titleStyle}>SimiliVec</h1>
        <p style={descriptionStyle}>
          Explore high-dimensional vector spaces with interactive 3D visualization
        </p>
        <button onClick={onContinue} className='continue-button'>
          Continue
        </button>
        <div className="home-socials">
          {githubUrl && (
            <a href={githubUrl} target="_blank" rel="noopener noreferrer" aria-label="GitHub">
              <img src={githubLogo} alt="Github" />
            </a>
          )}
          {linkedinUrl && (
            <a href={linkedinUrl} target="_blank" rel="noopener noreferrer" aria-label="LinkedIn">
              <img src={linkedinLogo} alt="LinkedIn" />
            </a>
          )}
        </div>
      </div>
    </div>
  );
}

const homeScreenStyle = {
  width: '100%',
  height: '100vh',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  background: 'linear-gradient(135deg, #1a1a1a 0%, #2d2d2d 100%)',
  color: 'white',
};

const contentStyle = {
  textAlign: 'center',
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  gap: '2rem',
};

const logoStyle = {
  width: 120,
  height: 120,
  objectFit: 'contain',
};

const titleStyle = {
  fontSize: '3rem',
  margin: 0,
  fontWeight: 'bold',
};

const descriptionStyle = {
  fontSize: '1.2rem',
  color: '#aaa',
  maxWidth: '400px',
};
